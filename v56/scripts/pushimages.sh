#!/bin/sh
#ADDR=192.168.1.75:5000
ADDR=lazyloki.ddns.net:5000
VER=56

sudo docker tag queenimage $ADDR/queenimage:Version$VER
sudo docker tag hiveimage $ADDR/hiveimage:Version$VER
sudo docker tag workerimage $ADDR/workerimage:Version$VER
sudo docker tag haproxyimage $ADDR/haproxyimage:Version$VER

sudo docker push $ADDR/queenimage:Version$VER
sudo docker push $ADDR/hiveimage:Version$VER
sudo docker push $ADDR/workerimage:Version$VER
sudo docker push $ADDR/haproxyimage:Version$VER


