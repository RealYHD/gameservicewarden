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
                sh "conda env update -n base --file environment.yml -q"
                sh "conda run -n base dotnet restore gameservicewarden.sln"
            }
        }
        stage ("Test") {
            steps {
                sh returnStatus: true, script: 'conda run -n base dotnet test --logger xunit --no-restore gameservicewarden.sln'
                xunit([xUnitDotNet(excludesPattern: '', pattern: 'GameServiceWarden.Tests/*.Tests/TestResults/*.xml', stopProcessingIfError: true)])
            }
        }
    }
}

