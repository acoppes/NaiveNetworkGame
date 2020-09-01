pipeline {
    agent any

    options { 
        skipDefaultCheckout()
    }

    environment {
        U3D_EXTRA_PATHS = "/mnt/c/UnityHub/"
        UNITY_EXECUTABLE_PATH = "/mnt/c/UnityHub/2020.2.0a21/Editor/Unity.exe"
        // ${env.WORKSPACE}
        WORKSPACE_CLIENT = "/mnt/d/Temp/jenkins_tmp_workspace/Client"
        WORKSPACE_SERVER = "/mnt/d/Temp/jenkins_tmp_workspace/Server"
    }

    triggers { 
        pollSCM('H/30 * * * *') 
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
                    /* environment {
                        LOG_FILE = "Server/Logs/build-server-linux.log"
                        // PROJECT_PATH = "${WORKSPACE}/Server"
                    } */
                    stages {
                        stage('Checkout') {
                            steps {
                                echo "checkout to ${WORKSPACE}"
                                checkout scm                      
                            }
                        }
                        stage('Build') {
                            steps echo "step"
                            
                            /* steps {
                                sh "./build-server-linux.sh" 
                                sh "./stop_remote_server.sh" 
                                sh "./deploy_server.sh" 
                                sh "./start_remote_server.sh"
                            }  */                     
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
                    /*environment {
                        LOG_FILE = "Client/Logs/build-client-windows.log"
                        // PROJECT_PATH = "${WORKSPACE}/Client"
                    } */
                    stages {
                        stage('Checkout') {
                            steps {
                                echo "checkout to ${WORKSPACE}"
                                checkout scm                      
                            }
                        }
                        stage('Build') {
                            steps {
                                echo "Building client..."
                                //sh "./build-client-windows.sh"
                                // sh "./deploy_clients.sh"
                            }                         
                        }
                        stage('Deploy') {
                            steps {
                                echo "Deploying client"
                                // sh "./deploy_clients.sh"
                            }                       
                        }
                    }
                }         
            } 
        }
    }
}