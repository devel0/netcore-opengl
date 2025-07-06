FROM ubuntu:noble

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get -y update

# basic utils
RUN apt-get update && \
    apt-get install -y apt-utils apt-transport-https && \
    apt-get install -y curl wget jq nano net-tools inetutils-ping iproute2 psmisc

ENV EDITOR=nano

# xvfb
RUN apt-get install -y xvfb

# net9.0
RUN apt-get install -y software-properties-common
RUN add-apt-repository ppa:dotnet/backports
RUN apt-get update && apt-get install -y dotnet9
