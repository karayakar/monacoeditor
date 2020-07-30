#
# A simple CSharpCompiler
#  
#

FROM microsoft/aspnetcore-build:latest
LABEL author="Karay Akar "
LABEL version=1.0
 

# Web API listens at http://localhost:5000 in container
EXPOSE 5000

# Copy web app directory to image
RUN mkdir -pv $HOMEDIR
WORKDIR $HOMEDIR
COPY . $HOMEDIR

RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
CMD ["dotnet", "run"]
