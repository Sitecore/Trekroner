$env:VERSION = (Get-Content ..\..\build.config.json | ConvertFrom-Json).Version
docker-compose build
docker-compose up -d