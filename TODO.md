
# Project Trekroner TODO
* [X] Add hosts writer for proxy 
  * https://github.com/RAhnemann/windows-hosts-writer/blob/master/Program.cs
* [X] Codefig for YARP
* [ ] Custom configuration (for topologies)
* [ ] Add configuration from docker-compose
* [ ] TLS from self-signed cert
* [ ] Error page for failed proxy
  * [ ] Basic error page for missing container or unable to connect
  * [ ] Add container logs to error page
  * [ ] Add container health to error page
  * [ ] Add dependency health / status to error page
* [ ] Wait / retry for starting containers
  * Can this be done in a user-friendly way that would also be compatible with CLI?
* [ ] Status page
  * [ ] Status for hosts writer
* [ ] Container actions on status page (e.g. restart)
* [ ] Notifications of issues via browser?

## ISSUES
* [ ] EOL with hosts file
* [ ] Hosts file - Need to remove before write on startup