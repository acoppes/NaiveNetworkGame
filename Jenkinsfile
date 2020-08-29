pipeline {
    agent any

    options { 
        skipDefaultCheckout()
    }

    environment {
        U3D_EXTRA_PATHS = "/c/UnityHub/"
        UNITY_EXECUTABLE_PATH = "/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
        WORKSPACE_CLIENT = "${env.WORKSPACE}/Client"
        WORKSPACE_SERVER = "${env.WORKSPACE}/Server"
    }

    triggers { 
        pollSCM('H H * * *') 
    }

    stages {
        stage ('Builds') {
            parallel {
                stage ('Server-Linux') {
                    agent {
                        label {
                            label ""
                            customWorkspace "${env.WORKSPACE_SERVER}"
                        }
                    }
                    environment {
                        // LOG_FILE = "Server/Logs/build-linux.log"
                        // PROJECT_PATH = "${WORKSPACE}/Server"
                    }
                    stages {
                        stage('Checkout') {
                            steps {
                                echo "checkout to ${WORKSPACE}"
                                checkout scm                      
                            }
                        }
                        stage('Build') {
                            steps {
                                sh "./build-server-linux.sh" 
                                sh "./deploy_server.sh"                           
                            }
                            /* post {
                                always {
                                   archiveArtifacts artifacts: "${LOG_FILE}"
                                }
                            }     */                       
                        }
                    }
                }  
                stage ('Client-Windows') {
                    agent {
                        label {
                            label ""
                            customWorkspace "${env.WORKSPACE_CLIENT}"
                        }
                    }
                    environment {
                        // LOG_FILE = "Client/Logs/build-windows.log"
                        // PROJECT_PATH = "${WORKSPACE}/Client"
                    }
                    stages {
                        stage('Checkout') {
                            steps {
                                echo "checkout to ${WORKSPACE}"
                                checkout scm                      
                            }
                        }
                        stage('Build') {
                            steps {
                                sh "./build-client-windows.sh"
                                sh "./deploy_clients.sh"
                            }
                            /* post {
                                always {
                                    archiveArtifacts artifacts: "${LOG_FILE}"
                                }
                            }  */                          
                        }
                    }
                }         
            } 
        }
    }
}