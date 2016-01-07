# Quorum
The purpose of this project is to provide a 'quorum' based implementation to facilitate reliable independent processes. 

For example, a quorum based Windows service could communicate with its peers to ensure there is only and always one 'master' service operating, thus avoiding the traditional single point of failure in Windows services. This would be useful if you had a Windows service where always and only one instance should be operating at any one time, but the service will be redudantly deployed across multiple machines in a web farm.

This project provides:

* An MVC web app for quorum configuration, monitoring and real time log capture
* A pro forma Windows service implementation for a quorum member
* A pro forma console app implementation for a quorum member

Please see the [wiki](https://github.com/afgbeveridge/Quorum/wiki) for further details.

# License
This project is released under the [MIT license](https://opensource.org/licenses/MIT), with the project specific license [here](https://github.com/afgbeveridge/Quorum/blob/master/license.md)
