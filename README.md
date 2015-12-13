# Quorum
The purpose of this project is to provide a 'quorum' based implementation to facilitate reliable independent processes. 

For example, a quorum based Windows service could communicate with its peers to ensure there is only and always one 'master' operating, thus avoiding the traditional single point of failure in Windows services. This would be useful if you had a Windows service where only one instance should be operating at any one time, but the service will be redudantly deployed across multiple machines in a web farm.

Please see the [wiki](https://github.com/afgbeveridge/Quorum/wiki) for further details.

# License
This project is released under the [MIT license](https://opensource.org/licenses/MIT)
