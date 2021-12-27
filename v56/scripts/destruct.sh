#!/bin/sh
sudo docker service rm worker
sudo docker service rm hive
sudo docker service rm queen
sudo docker service rm haproxy
sudo docker network rm galaxynet