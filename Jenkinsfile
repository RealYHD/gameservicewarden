pipeline {
    agent {
        kubernetes {
            cloud 'Reslate Systems'
            defaultContainer 'conda'
        }
    }
    stages {
        stage ("Install") {
            steps {
                sh "conda update conda -y -q"
                sh "conda env update -n base --file environment.yml"
                sh "conda run -n base dotnet restore gameservicewarden.sln"
            }
        }
        stage ("Test") {
            steps {
                sh "conda run -n base dotnet test --logger xunit --no-restore GameServiceWarden.Tests"
                xunit([xUnitDotNet(excludesPattern: '', pattern: 'GameServiceWarden.Tests/*.Tests/TestResults/*.xml', stopProcessingIfError: true)])
            }
        }
    }
}

