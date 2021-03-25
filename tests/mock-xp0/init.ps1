docker-compose build
docker-compose run --rm -T --entrypoint "cmd /c c:\\get-cert-script.bat" ingress
.\docker\https\New-HttpsCertificate.ps1 -Domain *.basic-test.test -CertPassword "b"