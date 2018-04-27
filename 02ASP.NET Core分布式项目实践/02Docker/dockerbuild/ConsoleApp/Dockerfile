FROM microsoft/dotnet
WORKDIR /app
COPY . /app
CMD ["dotnet","run"]


#FROM micrsoft/dotnet:sdk as build-env
#WORKDIR /code
#COPY *.csproj /code
#RUN dotnet restore
#
#COPY . /code
#RUN dotnet publish -c Release -o out
#FROM microsoft/dotnet:runtime
#WORKDIR /app
#COPY --from=build-dev /code/out /app
#ENTRYPOINT ["dotnet","console.dll"]