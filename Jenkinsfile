pipeline {
    agent any
    stages {
        stage ("Install") {
            sh "mamba env update --file environment.yml"
            sh "echo 'mamba activate props' >> ~/.bashrc"
            sh "dotnet restore GameServiceWarden"
            sh "dotnet restore GameServiceWarden.Tests"
        }
        stage ("Test") {
            sh "sh 'dotnet test --logger xunit --no-restore GameServiceWarden.Tests"
            xunit([xUnitDotNet(excludesPattern: '', pattern: 'GameServiceWarden.Tests/*.Tests/TestResults/*.xml', stopProcessingIfError: true)])
        }
    }
}

