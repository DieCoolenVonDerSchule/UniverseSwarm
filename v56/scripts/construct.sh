#!/bin/sh
sudo docker network create -d macvlan --scope swarm --subnet 10.11.0.0/24 -o parent=enp0s3.10 --ip-range 10.11.0.0/24 galaxynet
sudo docker service create --network galaxynet --name haproxy --replicas=1 --constraint node.role==manager -p published=5551,target=5051,protocol=tcp -p published=555,target=5050,protocol=tcp lazyloki.ddns.net:5000/haproxyimage:Version54
sudo docker service create --network galaxynet --name worker --replicas=5 lazyloki.ddns.net:5000/workerimage:Version45
sudo docker service create --network galaxynet --name queen --replicas=1 --constraint node.role==manager lazyloki.ddns.net:5000/queenimage:Version54
sudo docker service create --network galaxynet --name hive -e QUEEN_ADDR=queen -e WORKER_ADDR=worker --replicas=2 lazyloki.ddns.net:5000/hiveimage:Version54