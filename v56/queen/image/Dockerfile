FROM alpine:latest
MAINTAINER DCVDS

RUN apk --no-cache upgrade
    
ENV PYTHONUNBUFFERED=1
RUN apk add --update --no-cache python3 && ln -sf python3 /usr/bin/python
RUN python3 -m ensurepip
RUN pip3 install --no-cache --upgrade pip setuptools
RUN pip install websockets
RUN apk add --update --no-cache py3-numpy
ENV PYTHONPATH=/usr/lib/python3.8/site-packages

EXPOSE 5050

WORKDIR /home
COPY command.sh /home
COPY queen.py /home

ENTRYPOINT ["/bin/sh", "/home/command.sh"]
