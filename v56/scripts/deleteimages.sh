#!/bin/sh
#ADDR=192.168.1.75:5000
ADDR=lazyloki.ddns.net:5000
VER=56

sudo docker image rm $ADDR/queenimage:Version$VER
sudo docker image rm $ADDR/hiveimage:Version$VER
sudo docker image rm $ADDR/workerimage:Version$VER
sudo docker image rm $ADDR/haproxyimage:Version$VER