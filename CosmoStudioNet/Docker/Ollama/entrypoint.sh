#!/bin/bash
set -e

MODEL="qwen2.5:32b-instruct-q4_1"

echo "Comprobando si el modelo $MODEL existe..."
if ! ollama list | grep -q "$MODEL"; then
  echo "Modelo no encontrado, descargando..."
  ollama pull "$MODEL"
else
  echo "El modelo $MODEL ya está instalado."
fi

echo "Iniciando servidor Ollama..."
exec ollama serve
