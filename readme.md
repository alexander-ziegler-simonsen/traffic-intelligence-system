# What is 'traffic-intelligence-system'?

The project 'Traffic Intelligence System' (shortened to TIS) aims to keep track of public and private transport, in order to improve public transport, reduce accidents, and inform drivers about traffic problems.

This project is my final education project. It is not connected to any real company, so any connection to real companies in the future is not intentional.

# What stack is used?

client - node, react, vite
api - dotnet web api, entity framework
databases - Postgres (write), MongoDB (read), Redis (memory simulator data)
services - Martin (map tile render)
.........

# How can I run TIS on my PC?

first you will need Docker or Docker desktop in order to run it locally. 
Before hand you need to fill out some `.env` values, you can make a copy of the `.env.temp` file and rename it to `.env` and fill out each line.

then yuo have to run the 2 commands in the file `setupCommandsToRun.txt`, which you will have to do from root inside your terminal.
This first build the custom docker image, afterwards it runs it.
Both commands can take a lot of time, maybe 30-45 mins, based on your pc and network speed.

When you run the docker image, it will download some datasets, which then gets treated and afterwards the container will build the files that project needs. 
the datasets are kind of big, so you will need around 1,5 - 2 gb of space.
After it is done, you can delete the bigger files and keep the new small ones.




