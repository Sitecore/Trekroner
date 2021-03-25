[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "", Justification="For development only")]
Param (
    [Parameter(Mandatory = $true)]
    [string]$Domain,
    [Parameter(Mandatory = $true)]
    [string]$CertPassword
)

$ErrorActionPreference = 'Stop'
$rootCertName = "Sitecore Trekroner Trusted Root"
$rootCertFileBasePath = "$env:UserProfile\.sitecore\trekroner-rootCA";
$rootCertExportPath = "$rootCertFileBasePath.pfx"
$rootCertPemPath = "$rootCertFileBasePath.pem"
$certBaseParams = @{
    CertStoreLocation = 'Cert:\LocalMachine\My'
    Subject = 'CN=Sitecore Trekroner, O=DO_NOT_TRUST, OU=Created by https://www.github.com/Sitecore/Trekroner'
    KeyLength = 2048
    KeyAlgorithm = 'RSA'
    HashAlgorithm = 'SHA256'
    KeyExportPolicy = 'Exportable'
    NotAfter = (Get-Date).AddYears(20)
}

function Export-PemCertificate($Cert, $FilePath) {
    $InsertLineBreaks=1
    $oPem=new-object System.Text.StringBuilder
    $oPem.AppendLine("-----BEGIN CERTIFICATE-----") | out-null
    $oPem.AppendLine([System.Convert]::ToBase64String($cert.RawData,$InsertLineBreaks)) | out-null
    $oPem.AppendLine("-----END CERTIFICATE-----") | out-null
    $oPem.ToString() | out-file $FilePath
}

# Do we already have a Trusted Root cert?
$rootCert = Get-ChildItem cert:\LocalMachine\Root | Where-Object { $_.FriendlyName -eq $rootCertName }

if (-not $rootCert) {
    Write-Host "$rootCertName not found, Creating trusted root certificate."
    $rootCertParams = $certBaseParams.Clone()
    $rootCertParams.DnsName = 'DO_NOT_TRUST_SitecoreTrekronerRootCert'
    $rootCertParams.FriendlyName = $rootCertName
    $rootCertParams.KeyUsage = 'CertSign','CRLSign'

    $tempRootCert = New-SelfSignedCertificate @rootCertParams
    Write-Host "Exporting root certificate to $rootCertExportPath"
    Export-PfxCertificate -Cert $tempRootCert -FilePath $rootCertExportPath -Password (ConvertTo-SecureString -Force -AsPlainText $CertPassword) | Out-null
    Write-Host "Exporting node-friendly root certificate to $rootCertPemPath"
    Export-PemCertificate -Cert $tempRootCert -FilePath $rootCertPemPath

    $rootCert = Import-PfxCertificate -CertStoreLocation 'Cert:\LocalMachine\Root' -FilePath $rootCertExportPath -Password (ConvertTo-SecureString -Force -AsPlainText $CertPassword)
    $rootCert.FriendlyName = $rootCertName
    Write-Host "Root certificate installed."
} else {
    Write-Host "Trusted root $rootCertName found at $($rootCert.PSParentPath)"
    if (Test-Path -Path $rootCertExportPath) {
        Write-Host "Existing exported root cert found at $rootCertExportPath"
    }
    if (Test-Path -Path $rootCertPemPath) {
        Write-Host "Existing exported node-friendly root certificate found at $rootCertPemPath"
    }
}

$certParams = $certBaseParams.Clone()
$certParams.DnsName = $Domain
$certParams.Subject = "CN=Sitecore Trekroner $Domain, O=DO_NOT_TRUST, OU=Created by https://www.github.com/Sitecore/Trekroner"
$certParams.Signer = $rootCert

Write-Host "Creating certificate for $Domain"
$cert = New-SelfSignedCertificate @certParams

$certFile = Join-Path $PSScriptRoot "$($Domain.Replace("*", "_")).pfx"
Write-Host "Exporting certificate to $certFile"
Export-PfxCertificate -Cert $cert -FilePath $certFile -Password (ConvertTo-SecureString -Force -AsPlainText $CertPassword) | out-null
$cert | Remove-Item