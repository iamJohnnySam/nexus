# nexus Installation Guide

```
sudo apt-get update
sudo apt-get upgrade
sudo apt-get update
```
## LCD Screen
https://forums.raspberrypi.com/viewtopic.php?t=238060
```
sudo rm -rf LCD-show
git clone https://github.com/goodtft/LCD-show.git
chmod -R 755 LCD-show
cd LCD-show/
sudo ./LCD35-show
```

Rotate LCD
```
cd LCD-show/
sudo ./rotate.sh 180
```

## Git
```
sudo apt-get install git -y
```
```
git clone https://github.com/iamJohnnySam/nexus.git
```
```
git config --global --add safe.directory /home/pi/nexus
sudo git config --global --add safe.directory /home/pi/nexus
```

## Python
```
python -m venv nexus
cd nexus
source bin/activate
```
```
pip install numpy
```

## Samba
```
sudo apt-get install samba samba-common-bin -y
sudo apt install cifs-utils -y
```
```
sudo nano /etc/samba/smb.conf
```
```
[nexus]
path = /home/nexus
writeable=Yes
create mask=0777
directory mask=0777
public=no
```
```
sudo smbpasswd -a pi
```
```
sudo systemctl restart smbd
```

## LaTeX Compiler
```
sudo apt-get update
sudo apt-get upgrade
sudo apt-get install kile
sudo apt-get install okular
```

## SQL
```
sudo apt install mariadb-server -y
sudo apt install phpmyadmin -y
```
```
sudo mysql_secure_installation
```
Config Installation
```
sudo mysql -u root -p
```
```
CREATE USER 'nexus'@'localhost' IDENTIFIED BY 'xxxxxx';
GRANT ALL PRIVILEGES ON *.* TO 'nexus'@'localhost';
```
```
GRANT ALL PRIVILEGES ON *.* TO 'nexus'@'localhost' IDENTIFIED BY 'xxxxxx' WITH GRANT OPTION;
FLUSH PRIVILEGES;
```
ctrl+q exit
```
sudo nano /etc/apache2/apache2.conf
```
Add to bottom
```
Include /etc/phpmyadmin/apache.conf
```
```
sudo service apache2 restart
sudo ln -s /usr/share/phpmyadmin /var/www/html
```

## .NET8
```
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel STS
```
```
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.404
```
```
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
```
```
dotnet --version
```
---
# nexus Configuration

```
```
```
```
```
```