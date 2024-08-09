# Certificate demos

Certificate demos

## Certificate validation example

Generate as many Root CAs as you need and then certificates from those CAs:

```powershell
# Create Root CA
$app = "2"
$rootCA = New-SelfSignedCertificate `
  -Subject "DemoApp$($app)RootCA" `
  -FriendlyName "Demo App$($app) Root CA" `
  -CertStoreLocation "cert:\CurrentUser\My" `
  -NotAfter (Get-Date).AddYears(20) `
  -KeyUsageProperty All -KeyUsage CertSign, CRLSign, DigitalSignature

$password = ConvertTo-SecureString -String "1234" -Force -AsPlainText

Get-ChildItem -Path cert:\CurrentUser\my\$($rootCA.Thumbprint) | 
  Export-PfxCertificate -FilePath DemoApp$($app)RootCA.pfx -Password $password

Export-Certificate -Cert cert:\CurrentUser\my\$($rootCA.Thumbprint) -FilePath DemoApp$($app)RootCA.crt

# Create Intermediate Certificate
$intermediateCertificate = New-SelfSignedCertificate `
  -CertStoreLocation cert:\CurrentUser\my `
  -Subject "DemoApp$($app) Intermediate certificate" `
  -FriendlyName "DemoApp$($app) Intermediate certificate" `
  -Signer $rootCA `
  -NotAfter (Get-Date).AddYears(20) `
  -KeyUsageProperty All -KeyUsage CertSign, CRLSign, DigitalSignature `
  -TextExtension @("2.5.29.19={text}CA=1&pathlength=1")

$intermediatePassword = ConvertTo-SecureString -String "2345" -Force -AsPlainText

Get-ChildItem -Path cert:\CurrentUser\my\$($intermediateCertificate.Thumbprint) | 
  Export-PfxCertificate -FilePath DemoApp$($app)IntermediateCertificate.pfx -Password $intermediatePassword

Export-Certificate -Cert cert:\CurrentUser\my\$($intermediateCertificate.Thumbprint) -FilePath DemoApp$($app)IntermediateCertificate.crt
```

Use curl to invoke API with intermediate certificate:

```powershell
$app = "1"
$cert = Get-PfxCertificate ".\DemoApp$($app)IntermediateCertificate.pfx" -Password (ConvertTo-SecureString -String "2345" -Force -AsPlainText)

$parameters = @{
    Method  = "GET"
    Uri     = "https://localhost:7096/whoami"
    Certificate  = $cert
}
Invoke-RestMethod @parameters
# Example output:
# CN=DemoApp1 Intermediate certificate
```
