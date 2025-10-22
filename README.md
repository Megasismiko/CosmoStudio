# 🌌 CosmoStudio

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Docker](https://img.shields.io/badge/Docker-Ready-blue)
![Ollama](https://img.shields.io/badge/Ollama-LLM-green)
![Kokoro](https://img.shields.io/badge/Kokoro-TTS-orange)
![StableDiffusion](https://img.shields.io/badge/StableDiffusion-ImageGen-purple)
![License](https://img.shields.io/badge/License-MIT-lightgrey)

**CosmoStudio** es una plataforma modular para la **generación automatizada de contenido multimedia** (guiones, voz, imágenes y video) mediante modelos locales e integraciones IA autoalojadas.  
El objetivo es permitir la creación de videos completos a partir de un tema o idea, combinando inteligencia artificial y una arquitectura extensible.

---

## 🚀 Características principales

- **Generación de guiones** en inglés y español usando *Ollama* (modelos LLM locales).
- **Revisión automática** del guion mediante un proceso de post-edición y corrección.
- **Síntesis de voz (TTS)** con *Kokoro FastAPI*, compatible con GPU y modelos multilenguaje.
- **Generación de imágenes** con *Stable Diffusion WebUI*.
- **Almacenamiento estructurado** (local o en la nube) mediante `StorageOptions`.
- **Arquitectura modular** con capas separadas (DAL, BLL, API).
- **Despliegue completo en Docker** para entorno de producción o desarrollo.

---

## 🧩 Arquitectura

```
CosmoStudio/
├── CosmoStudio.WebApi/           # API principal (controladores REST)
├── CosmoStudio.BLL/              # Lógica de negocio (servicios)
│   ├── Servicios/
│   │   ├── GuionServicio.cs      # Generación y revisión de guiones
│   │   ├── ImagenService.cs      # Generación de imágenes
│   │   ├── VozService.cs         # Generación de audio con Kokoro
│   │   └── ...
│   ├── Ollama/                   # Cliente de conexión con Ollama
│   └── StableDiffusion/          # Cliente HTTP para imágenes
├── CosmoStudio.DAL/              # Capa de datos (EF Core)
│   ├── CosmoDbContext.cs
│   └── Entidades/
│       ├── Proyecto.cs
│       ├── Guion.cs
│       └── GuionVersion.cs
├── CosmoStudio.Common/           # Modelos y DTOs
└── docker-compose.yml            # Orquestación de contenedores
```

---

## ⚙️ Configuración

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "sqlConnection": "Server=localhost;Database=CosmoDB;Trusted_Connection=True;"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama3",
    "TimeoutSeconds": 1200
  },
  "Kokoro": {
    "BaseUrl": "http://localhost:8880"
  },
  "StableDiffusion": {
    "BaseUrl": "http://localhost:7860"
  },
  "Storage": {
    "RootPath": "C:\\CosmoStudioStorage",
    "Provider": "Local"
  }
}
```

---

## 🐳 Ejecución con Docker

```bash
docker compose up -d
```
Esto levantará los servicios:
- `cosmostudio-api` → API principal
- `ollama` → modelo LLM
- `kokoro` → servidor TTS
- `stable-diffusion` → generador de imágenes

Para detenerlos:
```bash
docker compose down
```

---

## 🧠 Flujo de generación de contenido

1. El usuario crea un **proyecto** indicando un tema.
2. `GuionServicio` genera un **outline** con Ollama.
3. Se generan los **guiones por sección**, traducidos y revisados.
4. `VozService` genera la **voz** con Kokoro.
5. `ImagenService` crea **imágenes** con Stable Diffusion.
6. Se guardan todos los recursos en el almacenamiento configurado.

---

## 🔌 Endpoints principales

| Método | Endpoint | Descripción |
|--------|-----------|-------------|
| `POST` | `/api/proyectos` | Crear un nuevo proyecto |
| `GET` | `/api/proyectos/ultimos?top=5` | Obtener los últimos proyectos |
| `POST` | `/api/guion/outline` | Generar el outline inicial |
| `POST` | `/api/guion/script` | Generar el guion completo |
| `POST` | `/api/voz/generar` | Generar la voz TTS |
| `POST` | `/api/imagen/generar` | Generar una imagen |

---

## 🧪 Ejemplo de uso

```http
POST /api/guion/outline
Content-Type: application/json

{
  "titulo": "Agujeros Negros",
  "tema": "Cosmología y física cuántica"
}
```

**Respuesta:**
```json
{
  "titulo": "Agujeros Negros",
  "numSecciones": 50,
  "duracion": "60min",
  "tiempoEjecucion": "38s"
}
```

---

## 🔧 Tecnologías principales

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core**
- **Ollama** (modelos LLM locales)
- **Kokoro FastAPI** (TTS)
- **Stable Diffusion WebUI API**
- **Docker & Docker Compose**

---

## 📁 Estructura del almacenamiento

```
/storage
├── proyectos/
│   ├── {id}/
│   │   ├── outline_en.txt
│   │   ├── outline_es.txt
│   │   ├── script_en.txt
│   │   ├── script_es.txt
│   │   ├── audio/
│   │   ├── imagenes/
│   │   └── metadatos.json
```

---

## 👩‍💻 Autores y mantenimiento

Proyecto desarrollado por el equipo **CosmoStudio**  
🧠 Integración IA y arquitectura: *[Tu nombre o equipo]*  
📦 Repositorio base: privado / en preparación

---

## 🪐 Próximas mejoras

- Integración de **video automático** (voz + imagen).
- Interfaz web para gestión de proyectos.
- Control de versiones de guiones.
- Modo colaborativo multiusuario.

---

## 📄 Licencia

Este proyecto se distribuye bajo licencia **MIT**.
