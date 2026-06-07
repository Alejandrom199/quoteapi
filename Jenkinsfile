pipeline {
    agent any

    // Variables de entorno del pipeline
    environment {
        // Cambia 'tu-usuario' por tu usuario real de Docker Hub
        IMAGEN = 'tu-usuario/quoteapi'
        // BUILD_NUMBER es automático de Jenkins (incrementa cada ejecución)
        TAG = "1.0.${BUILD_NUMBER}"
    }

    parameters {
        booleanParam(
            name: 'PUBLICAR',
            defaultValue: false,
            description: '¿Publicar la imagen al registry?'
        )
    }

    stages {
        // ── Stage 1: Compilar y testear dentro de un contenedor .NET ──
        stage('Build y Test') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:8.0'
                    // reuseNode mantiene el workspace (los archivos) entre stages
                    reuseNode true
                }
            }
            steps {
                dir('QuoteApi') {       // 'dir' cambia al subdirectorio
                    sh 'dotnet restore'
                    sh 'dotnet build --configuration Release --no-restore'
                    // Si tuvieras tests: sh 'dotnet test'
                }
            }
        }

        // ── Stage 2: Construir la imagen Docker ──
        stage('Construir imagen') {
            steps {
                dir('QuoteApi') {
                    // Construye la imagen con dos tags: la versión y 'latest'
                    sh "docker build -t ${IMAGEN}:${TAG} -t ${IMAGEN}:latest ."
                }
                echo "Imagen construida: ${IMAGEN}:${TAG}"
            }
        }

        // ── Stage 3: Probar que la imagen arranca (smoke test) ──
        stage('Smoke test de la imagen') {
            steps {
                // Levantamos el contenedor (no hace falta exponer el puerto al host con -p)
                sh """
                    docker run -d --name quote-smoke ${IMAGEN}:${TAG}
                    sleep 5
                    
                    # Extraemos la IP interna del contenedor
                    CONTAINER_IP=\$(docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' quote-smoke)
                    
                    # Hacemos curl directo a la IP interna y puerto original (8080)
                    curl -f http://\$CONTAINER_IP:8080/health || {
                        echo "❌ El healthcheck falló. Mostrando logs del contenedor para debug:"
                        docker logs quote-smoke
                        exit 1
                    }
                    
                    docker stop quote-smoke
                    docker rm quote-smoke
                """
                echo '✅ La imagen arranca y responde correctamente'
            }
        }

        // ── Stage 4: Publicar (solo si el parámetro PUBLICAR es true) ──
        stage('Publicar imagen') {
            when {
                expression { params.PUBLICAR == true }
            }
            steps {
                withCredentials([usernamePassword(
                    credentialsId: 'dockerhub-creds',
                    usernameVariable: 'DOCKER_USER',
                    passwordVariable: 'DOCKER_PASS'
                )]) {
                    sh 'echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin'
                    sh "docker push ${IMAGEN}:${TAG}"
                    sh "docker push ${IMAGEN}:latest"
                }
                echo "Imagen publicada: ${IMAGEN}:${TAG}"
            }
        }
    }

    post {
        always {
            // Limpieza: elimina las imágenes locales para no llenar el disco
            sh "docker rmi ${IMAGEN}:${TAG} ${IMAGEN}:latest || true"
        }
        success {
            echo "Pipeline completado. Versión: ${TAG}"
        }
        failure {
            // Limpieza defensiva por si el smoke test dejó un contenedor colgado
            sh 'docker rm -f quote-smoke || true'
            echo 'El pipeline falló'
        }
    }
}