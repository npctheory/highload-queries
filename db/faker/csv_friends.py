import csv
import os
import random

# Define the CSV file paths
users_csv_file = './db/csv/fake_users.csv'
friends_csv_file = './db/csv/fake_friends.csv'

# Ensure the directory exists
os.makedirs(os.path.dirname(friends_csv_file), exist_ok=True)

# Read the user IDs from the CSV file
user_ids = []
with open(users_csv_file, mode='r') as file:
    reader = csv.DictReader(file)
    for row in reader:
        user_ids.append(row['id'])

# Generate friends relationships
friendships = []
for user_id in user_ids:
    friends = random.sample(user_ids, 200)
    # Ensure a user does not friend themselves
    friends = [friend_id for friend_id in friends if friend_id != user_id]
    friendships.extend([(user_id, friend_id) for friend_id in friends])

# Write the friendships to the new CSV file
with open(friends_csv_file, mode='w', newline='') as file:
    writer = csv.writer(file)
    # Write the header
    writer.writerow(['user_id', 'friend_id'])
    # Write the friendship data
    writer.writerows(friendships)

print(f'Fake friendships have been written to {friends_csv_file}')
