param([Parameter(Mandatory=$true)][string]$i, [Parameter(Mandatory=$true)][string]$o)
$inputStream = New-Object System.IO.FileStream($i, [System.IO.FileMode]::Open)
$outputStream = New-Object System.IO.FileStream($o, [System.IO.FileMode]::Create)
$deflateStream = New-Object System.IO.Compression.DeflateStream($outputStream, [System.IO.Compression.CompressionMode]::Compress)
$inputStream.CopyTo($deflateStream)
$deflateStream.Close()
$inputStream.Close()