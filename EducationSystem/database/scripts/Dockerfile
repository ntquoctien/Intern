FROM mcr.microsoft.com/mssql-tools18

WORKDIR /scripts

# Copy database scripts and entrypoint shell script
COPY . .

# Grant execution permissions
RUN chmod +x /scripts/init-db.sh

ENTRYPOINT ["/scripts/init-db.sh"]
