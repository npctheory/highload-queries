## О проекте
Домашнее задание по очередям.  
Проект состоит из следующих компонентов:  
* Приложение .NET WebApi в папке ./server, которое собирается в образ server:local и контейнер server.  
* Dockerfile и сид базы данных Postgres в папке ./db, которые собираются в образ db:local. Библиотекой Faker сгенерированы пользователи, френды, посты.
* В папке tests находятся запросы для расширения VSCode REST Client и экспорты коллекций и окружений Postman.
## Начало работы
Склонировать проект, сделать cd в корень репозитория и запустить Docker Compose.  
```bash
https://github.com/npctheory/highload-queries.git
cd highload-queries
docker compose up --build -d
```
## CRUDL методы контроллера постов 
При запуске Docker Compose и сборке образа db:local, в контейнер базы из папка db/initdb будет скопирован сид данных с заранее сгенерированными данными: 5000 пользователей (таблица Users). На каждого из пользователей заданы 200 случайных друзей (таблица Friendships). На каждого пользователя созданы 50 постов (таблица Posts). В сиде есть пользователь с id "LadyGaga", на которого подписаны все 5000 пользователей.  
В контроллере Core.Api.Controllers.PostController реализованы CRUDL REST-эндпоинты (post/create, post/get/{id}, post/update, post/delete/{post_id}, post/list) для работы с постами, эндпоинт post/feed для получения ленты постов друзей.  
Пример работы эндпоинтов PostController на видео:  

[CRUDL Post.webm](https://github.com/user-attachments/assets/defa25e5-e8b6-493c-9f63-e75cd1e0d242)


## Формирование ленты постов через постановку задачи в очередь  
Для асинхронного формирования/обновления кэша ленты пользователей используются классы пространства имен Core.Application.Posts.Queries.GetPostFeed: PostFeedCacheBuilder и FriendsPostFeedCacheRebuilder.  
Также хэндлер GetPostFeedQueryHandler самостоятельно синхронно формирует кэши ленты постов если по ключу в Редисе не найдены данные - создает кэш на первую 1000 постов если offset+limit не больше 1000, и кэширует все запросы свыше тысячи по динамическому ключу.  
Пространство имен EventBus.Events содержит классы событий, которые отправляются в in-memory шину MediatR или внешнюю шину RabbitMQ. Работа с RabbitMQ происходит через библиотеку MassTransit. 
При каждом успешном входе пользователя, добавлении или удалении друга создаются соответствующие события UserLoggedInEvent, FriendAddedEvent, FriendDeletedEvent которые отправляются в RabbitMQ, и из консюмеров MassTransit вызывают класс PostFeedCacheBuilder, который пересобирает кэш первой 1000 постов из ленты.  

Пример кэшировния ленты на первые 1000 сообщений и с динамическим ключом. При удалении друга все кэши пользователя удаляются по префиксу, а кэш на первые 1000 постов обновляется:  

[PostFeed Redis Cache.webm](https://github.com/user-attachments/assets/dd25e1d3-4033-4165-a32a-4c14a2d6eb89)


## Эффект Celebrity
При каждом добавлении, удалении, изменении поста создаются события PostCreatedEvent, PostUpdatedEvent, PostDeletedEvent которые также отправляются в RabbitMQ, и из консюмеров MassTransit вызывают класс FriendsPostFeedCacheRebuilder, который получает список пользователей, добавивших автора поста в друзья, и заново создает кэш для ленты постов друзей каждому пользователю, у которого кэш не пустой.  
TTL ленты: 5 минут.  
На видео пользователь User формирует кэш ленты постов, пользователь LadyGaga добавляет новый пост - кэши ленты постов пользователя User очищаются по префиксу, кэш для первой тысячи перегенерируется:  

[Clear Post Feed Cache For Friends.webm](https://github.com/user-attachments/assets/3f258b86-b5c6-4514-a89c-193bdcb94517)


## WebSocket-Сервер  
На эндпоинте /post/feed/posted реализован SignalR-хаб PostHub, который отправляет подключенным пользователям события PostCreatedEvent, PostUpdatedEvent, PostDeletedEvent. Как и во всех контроллерах в хабе используется JWT-авторизация. Из клеймов извлекается идентификатор пользователя, для идентификатора из базы извлекается список друзей и на каждого друга создаются группа SignalR, в которые добавляется активное соединение. При возникновении событий изменения ленты постов - рассылка событий производится по группам SignalR.  
На видео пользователь User подключается к SignalR-хабу, а пользователь LadyGaga создает и удаляет новые посты:  


[SignalR Websockets.webm](https://github.com/user-attachments/assets/d655d6ee-8a30-4a38-bdad-910d39c17692)
