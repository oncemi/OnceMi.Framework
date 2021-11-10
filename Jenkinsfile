pipeline {
    agent any
	
	environment {
        DOCKER_IMAGE_NAME = 'oncemi_framework_api'
		EXPOSE_HTTP_PORT = '50000'
    }

    stages {
		stage('Prepare') {
            steps {
                // Build this project.
                sh 'echo Work directory is ${WORKSPACE}'
            }
        }
        stage('Build') {
            steps {
                sh 'docker build -f ${WORKSPACE}/src/OnceMi.Framework.Api/Dockerfile -t ${DOCKER_IMAGE_NAME}:latest --force-rm ${WORKSPACE}/src'
            }
        }
		stage('Deploy') {
            steps {
				sh '''\
				echo '#!/bin/bash' > stop_container.sh \
&& echo 'if [ "$(docker ps -a -q -f name='${DOCKER_IMAGE_NAME}')" ]; then' >> stop_container.sh \
&& echo '    echo "The container '${DOCKER_IMAGE_NAME}' is running, stopping..."' >> stop_container.sh \
&& echo '    docker stop '${DOCKER_IMAGE_NAME} >> stop_container.sh \
&& echo '    docker rm '${DOCKER_IMAGE_NAME} >> stop_container.sh \
&& echo 'fi' >> stop_container.sh
				'''
				sh 'chmod +x stop_container.sh'
				sh './stop_container.sh'
				sh 'rm -rf stop_container.sh'
                sh '''\
				docker run -itd --name ${DOCKER_IMAGE_NAME} \
 -p ${EXPOSE_HTTP_PORT}:80 \
 -e "ASPNETCORE_ENVIRONMENT=Production" \
 -e "ASPNETCORE_INITDB=true" \
 -v /etc/localtime:/etc/localtime \
 -v /data/oncemi/data:/oncemi/data \
 -v /data/oncemi/logs:/oncemi/logs \
 ${DOCKER_IMAGE_NAME}:latest
				'''
            }
        }
    }
}