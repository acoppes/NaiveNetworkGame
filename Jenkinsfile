pipeline {
    agent any

    options { 
        skipDefaultCheckout()
        // checkoutToSubdirectory('test-code') 
    }

    environment {
        U3D_EXTRA_PATHS = "/c/UnityHub/"
        UNITY_EXECUTABLE_PATH = "/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
        WORKSPACE_CLIENT = "${env.WORKSPACE}/Client"
        WORKSPACE_SERVER = "${env.WORKSPACE}/Server"
    }

    /*parameters {
        string(name: 'PERSON', defaultValue: 'Mr Jenkins', description: 'Who should I say hello to?')
    }*/

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
                        LOG_FILE = "build-server-linux.log"
                        // TESTS_LOG_FILE = "tests-android.xml"
                        PROJECT_PATH = "${WORKSPACE}/Server"
                    }
                    stages {
                        stage('Checkout') {
                            steps {
                                echo "checkout to ${WORKSPACE}"
                                checkout scm                      
                            }
                        }
                        /*stage('Test') {
                            steps {
                                sh "${UNITY_EXECUTABLE_PATH} -runTests -forgetProjectPath -projectPath \"${PROJECT_PATH}\" -batchmode -testResults \"${TESTS_LOG_FILE}\""
                            }
                            post {
                                always {
                                    archiveArtifacts artifacts: "MyProject/${TESTS_LOG_FILE}"
                                    nunit testResultsPattern: "MyProject/${TESTS_LOG_FILE}"
                                }
                            }                            
                        }*/
                        stage('Build') {
                            steps {
                                sh "${UNITY_EXECUTABLE_PATH} -buildTarget Linux64 -forgetProjectPath -projectPath \"${PROJECT_PATH}\" -quit -silent-crashes -batchmode -nographics -logfile \"${LOG_FILE}\" -executeMethod BuildScript.BuildLinux"
                            }
                            post {
                                always {
                                    archiveArtifacts artifacts: "${LOG_FILE}"
                                }
                            }                            
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
                        LOG_FILE = "build-client-windows.log"
                        TESTS_LOG_FILE = "tests-android.xml"
                        PROJECT_PATH = "${WORKSPACE}/Client"
                    }
                    stages {
                        stage('Checkout') {
                            steps {
                                echo "checkout to ${WORKSPACE}"
                                checkout scm                      
                            }
                        }
                        /* stage('Test') {
                            steps {
                                sh "${UNITY_EXECUTABLE_PATH} -runTests -forgetProjectPath -projectPath \"${PROJECT_PATH}\" -batchmode -testResults \"${TESTS_LOG_FILE}\""
                            }
                            post {
                                always {
                                    archiveArtifacts artifacts: "MyProject/${TESTS_LOG_FILE}"
                                    nunit testResultsPattern: "MyProject/${TESTS_LOG_FILE}"
                                }
                            }                            
                        } */
                        stage('Build') {
                            steps {
                                sh "${UNITY_EXECUTABLE_PATH} -buildTarget Win64 -forgetProjectPath -projectPath \"${PROJECT_PATH}\" -quit -silent-crashes -batchmode -nographics -logfile \"${LOG_FILE}\" -executeMethod BuildScript.BuildWindows"
                            }
                            post {
                                always {
                                    archiveArtifacts artifacts: "${LOG_FILE}"
                                }
                            }                            
                        }
                    }
                }         
            } 
        }
    }
}