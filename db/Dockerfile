FROM postgres:latest
USER root

# Install required packages including python3-venv and procps
RUN apt-get update && \
    apt-get install -y python3 python3-pip python3-venv procps && \
    rm -rf /var/lib/apt/lists/*

# Create a virtual environment and install psycopg2-binary
RUN python3 -m venv /opt/venv && \
    /opt/venv/bin/pip install --upgrade pip && \
    /opt/venv/bin/pip install psycopg2-binary requests==2.31.0

# Set the virtual environment's Python and Pip as default
ENV PATH="/opt/venv/bin:$PATH"

# Copy initialization scripts to the PostgreSQL container's initdb folder
COPY initdb /docker-entrypoint-initdb.d/

USER postgres
ENTRYPOINT ["docker-entrypoint.sh"]
EXPOSE 5432
CMD ["postgres"]
