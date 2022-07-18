# SampleServer
## Run with docker
```
# Change parent directory for copy dependencies.
cd ..

docker build -t echo-server -f SampleServer/Dockerfile .

docker run -p 3000:3000 -i echo-server
```
