FROM ubuntu:noble

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get -y update

# basic utils
RUN apt-get update && \
        apt-get install -y apt-utils apt-transport-https sudo curl wget gettext syslinux-utils bc jq man-db uuid && \
        apt-get install -y git ntpdate ntp supervisor cron rsyslog nano net-tools inetutils-ping telnet mc rsync dnsutils iproute2 psmisc acl systemd traceroute ldap-utils tcpdump && \
        apt-get install -y software-properties-common

ENV EDITOR=nano

# timezone
RUN apt-get install -y tzdata

# net9.0
RUN add-apt-repository ppa:dotnet/backports

RUN apt-get update && apt install -y dotnet-sdk-9.0

# xvfb
RUN apt-get install -y xvfb

RUN apt-get update && apt-get install -y libfontconfig-dev

# user mode

ARG USER_ID
ARG GROUP_ID

RUN groupmod -g 9999 ubuntu
RUN usermod -u 9999 ubuntu

RUN groupadd -g ${GROUP_ID} user
RUN useradd -m -l -u ${USER_ID} -g user user

RUN apt-get install -y sudo strace
RUN echo "user ALL=(ALL) ALL" >> /etc/sudoers

# package reference cache

RUN mkdir /.refs
COPY refs/Program.cs /.refs
COPY refs/refs.csproj /.refs
RUN chown -R user.user /.refs

RUN su -c "dotnet build /.refs" user

RUN dotnet build /.refs