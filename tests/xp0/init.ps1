[CmdletBinding()]
Param (
    [Parameter(Mandatory = $true)]
    [string]
    [ValidateNotNullOrEmpty()]
    $LicenseXmlPath,
    
    # We do not need to use [SecureString] here since the value will be stored unencrypted in .env,
    # and used only for transient local example environment.
    [string]
    $SitecoreAdminPassword = "Password12345",
    
    # We do not need to use [SecureString] here since the value will be stored unencrypted in .env,
    # and used only for transient local example environment.
    [string]
    $SqlSaPassword = "Password12345"
)

$ErrorActionPreference = "Stop";

if (-not (Test-Path $LicenseXmlPath)) {
    throw "Did not find $LicenseXmlPath"
}
if (-not (Test-Path $LicenseXmlPath -PathType Leaf)) {
    throw "$LicenseXmlPath is not a file"
}

# Check for Sitecore Gallery
Import-Module PowerShellGet
$SitecoreGallery = Get-PSRepository | Where-Object { $_.SourceLocation -eq "https://sitecore.myget.org/F/sc-powershell/api/v2" }
if (-not $SitecoreGallery) {
    Write-Host "Adding Sitecore PowerShell Gallery..." -ForegroundColor Green 
    Register-PSRepository -Name SitecoreGallery -SourceLocation https://sitecore.myget.org/F/sc-powershell/api/v2 -InstallationPolicy Trusted
    $SitecoreGallery = Get-PSRepository -Name SitecoreGallery
}
# Install and Import SitecoreDockerTools 
$dockerToolsVersion = "10.1.4"
Remove-Module SitecoreDockerTools -ErrorAction SilentlyContinue
if (-not (Get-InstalledModule -Name SitecoreDockerTools -RequiredVersion $dockerToolsVersion -ErrorAction SilentlyContinue)) {
    Write-Host "Installing SitecoreDockerTools..." -ForegroundColor Green
    Install-Module SitecoreDockerTools -RequiredVersion $dockerToolsVersion -Scope CurrentUser -Repository $SitecoreGallery.Name
}
Write-Host "Importing SitecoreDockerTools..." -ForegroundColor Green
Import-Module SitecoreDockerTools -RequiredVersion $dockerToolsVersion
Write-SitecoreDockerWelcome

###############################
# Populate the environment file
###############################

Write-Host "Populating required .env file variables..." -ForegroundColor Green

# SITECORE_ADMIN_PASSWORD
Set-EnvFileVariable "SITECORE_ADMIN_PASSWORD" -Value $SitecoreAdminPassword

# SQL_SA_PASSWORD
Set-EnvFileVariable "SQL_SA_PASSWORD" -Value $SqlSaPassword

# TELERIK_ENCRYPTION_KEY = random 64-128 chars
Set-EnvFileVariable "TELERIK_ENCRYPTION_KEY" -Value (Get-SitecoreRandomString 128)

# MEDIA_REQUEST_PROTECTION_SHARED_SECRET
Set-EnvFileVariable "MEDIA_REQUEST_PROTECTION_SHARED_SECRET" -Value (Get-SitecoreRandomString 64)

# SITECORE_IDSECRET = random 64 chars
Set-EnvFileVariable "SITECORE_IDSECRET" -Value (Get-SitecoreRandomString 64 -DisallowSpecial)

# SITECORE_ID_CERTIFICATE
$idCertPassword = Get-SitecoreRandomString 12 -DisallowSpecial
Set-EnvFileVariable "SITECORE_ID_CERTIFICATE" -Value (Get-SitecoreCertificateAsBase64String -DnsName "localhost" -Password (ConvertTo-SecureString -String $idCertPassword -Force -AsPlainText))

# SITECORE_ID_CERTIFICATE_PASSWORD
Set-EnvFileVariable "SITECORE_ID_CERTIFICATE_PASSWORD" -Value $idCertPassword

# SITECORE_LICENSE
Set-EnvFileVariable "SITECORE_LICENSE" -Value (ConvertTo-CompressedBase64String -Path $LicenseXmlPath)

##################################
# Configure TLS/HTTPS certificates
##################################

docker-compose build
docker-compose run --rm -T --entrypoint "cmd /c c:\\get-cert-script.bat" ingress
.\docker\https\New-HttpsCertificate.ps1 -Domain *.xp0-test.test -CertPassword "b"

Write-Host "Done!" -ForegroundColor Green