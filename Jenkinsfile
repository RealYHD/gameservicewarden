pipeline {
    agent any
    stages {
        stage ("Install") {
            steps {
                sh "mamba env update --file environment.yml"
                sh "echo 'mamba activate gameservicewarden' >> ~/.bashrc"
                sh "git submodule update --init --recursive"
                sh "dotnet restore GameServiceWarden"
                sh "dotnet restore GameServiceWarden.Tests"
            }
        }
        stage ("Test") {
            steps {
                sh "dotnet test --logger xunit --no-restore GameServiceWarden.Tests"
                xunit([xUnitDotNet(excludesPattern: '', pattern: 'GameServiceWarden.Tests/*.Tests/TestResults/*.xml', stopProcessingIfError: true)])
            }
        }
    }
}

