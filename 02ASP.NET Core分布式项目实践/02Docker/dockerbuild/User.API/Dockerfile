FROM microsoft/aspnetcore-build as build-env
WORKDIR /code
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=build-env /code/out ./
EXPOSE 80
#RUN dotnet User.API.dll
ENTRYPOINT ["dotnet","User.API.dll"]