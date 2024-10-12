FROM alex360/aspnet8-sdk:latest

# Set the working directory inside the container
WORKDIR /app

# Copy the application files to the container
COPY . .

# Restore dependencies
RUN dotnet restore

# Build the application
RUN dotnet build --no-restore -c Release

# Publish the application
RUN dotnet publish --no-restore -c Release -o out

# Set the entry point for the application
ENTRYPOINT ["dotnet", "out/YourApplication.dll"]
