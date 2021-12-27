#!/bin/sh
#INSTALL SCRIPT V.3 FOR USE WITH UBUNTU LINUX
sudo apt-get install docker.io
sudo apt-get install wireguard
sudo apt-get install resolvconf
sudo apt-get install net-tools
sudo wget lazyloki.ddns.net:8000/loki1.conf
sudo rm /etc/wireguard/wg0.conf
sudo cp loki1.conf /etc/wireguard/wg0.conf
sudo wg-quick up wg0
sudo echo '{"insecure-registries": ["lazyloki.ddns.net:5000"]}' > /etc/docker/daemon.json
sudo service docker restart
sudo socker swarm leave -f
sudo docker swarm join --token SWMTKN-1-3wl4xctje64r843uiln6j12pwsptyvrnqqot5sfryvb9514vn3-7dxu1vnfzskvjmi413u6s98h6 10.66.66.1:2377
