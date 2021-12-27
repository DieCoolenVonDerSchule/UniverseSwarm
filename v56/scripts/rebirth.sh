#!/bin/sh
sudo docker stack rm universeStack #killstack
sudo docker container stop $(sudo docker ps -a -q) && sudo docker container rm $(sudo docker ps -a -q) #killall
sudo docker image rm workerimage hiveimage queenimage #killimg
sudo sh ./scripts/deleteimages.sh #killreg
sudo docker image build -t queenimage queen/image && sudo docker image build -t hiveimage hive/image && sudo docker image build -t workerimage worker/image && sudo docker image build -t haproxyimage haproxy/image #buildimg
sudo sh ./scripts/pushimages.sh #pushimg
sudo docker stack deploy -c docker-compose.yml universeStack --with-registry-auth
