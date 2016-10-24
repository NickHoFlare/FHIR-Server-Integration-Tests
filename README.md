# FHIR-Server Integration Tests
This project was created as part of the work done for the thesis "Development of a Telehealth Framework for an Android Platform", by Nicholas Ho.

This is a suite of integration tests for the server component of the project.

To run:
1) Clone/download a copy of of this repository
2) Open the .sln file using Visual Studio 2015
3) Build the project.
	a) If there are any errors during the building process, close VS2015, navigate to the directory containing the source files and delete the "packages" folder (if present)
4) Open ANOTHER instance of VS2015 and open the FHIR Server project (https://github.com/terran324/FHIR-Server)
5) Run the FHIR server as per the instructions in the README.md of the server project
6) Run any test in this project.

NOTE: Integration tests are intended to be run individually. Some tests may modify the values in the server's database, which will affect success of subsequent consecutive tests. For best results, restart the server after running each integration test. (I know.)