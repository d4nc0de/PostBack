// Conexión a la base de datos 'biblioteca'
db = db.getSiblingDB('biblioteca');

// Colección de libros
db.createCollection('libros', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["titulo"],
      properties: {
        titulo: {
          bsonType: "string",
          description: "String no nulo"
        }
      }
    }
  }
});
// Índice único en el campo título
db.libros.createIndex({ titulo: 1 }, { unique: true });
// Inserción de datos iniciales
db.libros.insertMany([
  { titulo: "Cien años de soledad" },
  { titulo: "El amor en los tiempos del cólera" }
]);


// Colección de autores
db.createCollection('autores', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["nombre"],
      properties: {
        nombre: {
          bsonType: "string",
          description: "String no nulo"
        }
      }
    }
  }
});
// Índice único en el campo nombre
db.autores.createIndex({ nombre: 1 }, { unique: true });
// Inserción de datos iniciales
db.autores.insertMany([
  { nombre: "Gabriel García Márquez" },
  { nombre: "Mario Vargas Llosa" }
]);


// Colección escribe_libro (relación)
db.createCollection('escribe_libro', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["nombre_autor", "titulo_libro"],
      properties: {
        nombre_autor: {
          bsonType: "string",
          description: "String no nulo"
        },
        titulo_libro: {
          bsonType: "string",
          description: "String no nulo"
        }
      }
    }
  }
});
// Índice único en la combinación de nombre_autor y titulo_libro
db.escribe_libro.createIndex({ nombre_autor: 1, titulo_libro: 1 }, { unique: true });
// Inserción de datos iniciales
db.escribe_libro.insertMany([
  { nombre_autor: "Gabriel García Márquez", titulo_libro: "Cien años de soledad" },
  { nombre_autor: "Gabriel García Márquez", titulo_libro: "El amor en los tiempos del cólera" }
]);

// Colección de ediciones
db.createCollection('ediciones', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["ISBN", "año", "idioma", "titulo_libro"],
      properties: {
        ISBN: {
          bsonType: "string",
          description: "String no nulo"
        },
        año: {
          bsonType: "int",
          description: "Número entero positivo"
        },
        idioma: {
          bsonType: "string",
          description: "String no nulo"
        },
        titulo_libro: {
          bsonType: "string",
          description: "String no nulo"
        }
      }
    }
  }
});
// Índice único en el campo ISBN
db.ediciones.createIndex({ ISBN: 1 }, { unique: true });
// Inserción de datos iniciales
db.ediciones.insertMany([
  { ISBN: "978-3-16-148410-0", año: 1967, idioma: "español", titulo_libro: "Cien años de soledad" },
  { ISBN: "978-1-23-456789-7", año: 1985, idioma: "español", titulo_libro: "El amor en los tiempos del cólera" }
]);

// Colección de copias
db.createCollection('copias', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["numero", "ISBN"],
      properties: {
        numero: {
          bsonType: "int",
          description: "Número entero positivo"
        },
        ISBN: {
          bsonType: "string",
          description: "String no nulo"
        }
      }
    }
  }
});
// Índice único en la combinación de numero e ISBN
db.copias.createIndex({ numero: 1, ISBN: 1 }, { unique: true });
// Inserción de datos iniciales
db.copias.insertMany([
  { numero: 1, ISBN: "978-3-16-148410-0" },
  { numero: 2, ISBN: "978-3-16-148410-0" },
  { numero: 1, ISBN: "978-1-23-456789-7" }
]);

// Colección de usuarios
db.createCollection('usuarios', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["RUT", "nombre"],
      properties: {
        RUT: {
          bsonType: "string",
          description: "String no nulo"
        },
        nombre: {
          bsonType: "string",
          description: "String no nulo"
        }
      }
    }
  }
});
// Índice único en el campo RUT
db.usuarios.createIndex({ RUT: 1 }, { unique: true });
// Inserción de datos iniciales
db.usuarios.insertMany([
  { RUT: "12345678-9", nombre: "Carlos Morales" },
  { RUT: "98765432-1", nombre: "Ana Pérez" }
]);

// Colección de préstamos
db.createCollection('prestamos', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["RUT", "numero_copia", "fecha_prestamo"],
      properties: {
        RUT: {
          bsonType: "string",
          description: "String no nulo"
        },
        numero_copia: {
          bsonType: "int",
          description: "Número entero positivo"
        },
        fecha_prestamo: {
          bsonType: "date",
          description: "Fecha no nula"
        }
      }
    }
  }
});
// Índice único en la combinación de RUT y numero_copia
db.prestamos.createIndex({ RUT: 1, numero_copia: 1 }, { unique: true });
// Inserción de datos iniciales
db.prestamos.insertMany([
  { RUT: "12345678-9", numero_copia: 1, fecha_prestamo: new Date("2025-11-01"), fecha_devolucion: null },
  { RUT: "98765432-1", numero_copia: 2, fecha_prestamo: new Date("2025-10-28"), fecha_devolucion: new Date("2025-11-02") }
]);