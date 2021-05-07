$env:VERSION = (Get-Content ..\..\build.config.json | ConvertFrom-Json).Version
docker-compose -f .\docker-compose.yml -f .\docker-compose.debug.yml up -d