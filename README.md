# web-boot

dotnet web bootup utility

## Docker

Build an image

```
cd src/web-boot
docker build . -t mattjcowan/web-boot:0.0.1
```

Run it

```
docker run --rm -it -p 3000:80 mattjcowan/web-boot:0.0.1
```


