_Trekroner_ was an internal POC at Sitecore, that's since been abandoned. It's being archived here, should the community wish to fork and adopt it.

---

# Sitecore Trekroner ððð
_A reverse proxy for Docker-based Sitecore development._

## What is Trekroner?
Trekroner is an *experimental* reverse proxy based on [YARP](https://microsoft.github.io/reverse-proxy/), designed to replace the current use of Traefik in `docker-compose` based local Sitecore development. It is intended to solve some of the existing challenges with a typical Docker-based Sitecore development setup:

* The need to script or manually add `hosts` file entries. ð
* Installation and use of `mkcert` to generate certificates. ð
* The requirement to shut down IIS and other services on the ports which the Sitecore `docker-compose` files attempt to occupy. ð 
* Transparency of errors / issues within containers during startup, like `Bad gateway` and `ERROR: for traefik` messages. ð¡

It does this via:

â Making some basic assumptions about a typical Sitecore development setup with `docker-compose`.

â A simple environment variable-based configuration.

â Proxying of HTTP endpoints on specified containers using [Microsoft YARP](https://microsoft.github.io/reverse-proxy/), a netcore-based reverse proxy library.

â A `hosts` file writer which maps the IP of the proxy container to needed host names (and removes the entries on shutdown).

â Use of Kestrel's built-in support for `pfx` certificates, allowing generation of HTTPS certificates via PowerShell.

ð© _(TODO)_ Built-in container status reporting and log viewing when proxying fails.

ð© _(TODO)_ TCP proxying for SQL Server and other non-HTTP services.

## Usage
TODO

## FAQ

### Is this supported by Sitecore?
âNo.â

At this time Trekroner is an experimental "labs" project without support or a committed roadmap.

### Why does Sitecore need a reverse proxy at all for local development? Why Traefik?
The short answer is that same-site cookie rules for things like Identity and Analytics necessitate use of HTTPS, and it's easier to terminate HTTPS with a proxy than try to install certificates on all required containers. It's also more production-like. Traefik is a Docker-based proxy option which provides Windows-based containers, so it was a fit.

### Can't some of these issues be solved by a different Traefik or docker-compose configuration?
Yes, removal of some of the health checks, and a file-based Traefik configuration could also improve some of the issues mentioned above, but not all.

### How does hosts file writing solve port usage issues?
Every container has an IP address on a virtual network which can be accessed from the container host. These IPs change on every startup however, so port mappings are typically used to make the containers easier to access. By writing entries to the Windows `hosts` file, it becomes practical to access the containers directly.

### How does Trekroner's hosts writing compare to other tools?
Both [Windows Hosts Writer](https://github.com/RAhnemann/windows-hosts-writer) and [Whales Names](https://github.com/gregolsky/whales-names) monitor the Docker Engine and add entries for every container. In its current implementation, Trekroner assumes it will be proxying every request into the container network, so it only maps its own IP.

### Trekroner aims to do a lot, shouldn't these be separate microservice containers? Sidecar containers?
Do you really need more containers in your Sitecore environment? ð

Seriously though, there are some benefits to combining these. The hosts writer can be smart about host names that need to be proxied, and the status check / error reporting can be integrated into the proxy error handling. As it's intended for development usage, independent scaling isn't a concern.

### What is a 'Trekroner'?
[Trekroner Sea Fort](https://en.wikipedia.org/wiki/Trekroner_Fort) ("three crowns") is a sea fort outside Copenhagen, which should look familiar to many Sitecore developers, as the home of the lighthouse which graced the Sitecore Desktop for many years.

You can check out a [photo tour of Trekroner Fort](http://www.codeflood.net/blog/2016/05/01/trekroner-fort/) by Sitecore architect Alistair Deneys (@adeneys).


## Contributing
TODO
