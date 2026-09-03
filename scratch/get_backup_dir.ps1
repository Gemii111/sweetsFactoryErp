$conn = New-Object System.Data.SqlClient.SqlConnection('Server=192.168.1.90;Database=master;User Id=sa;Password=Aa456456;TrustServerCertificate=True;')
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(500)) AS BackupPath"
$path = $cmd.ExecuteScalar()
Write-Host "Default Backup Path: $path"
$conn.Close()
