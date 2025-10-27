// Relaciones esperadas:
// (USUARIO)-[:PUBLICA]->(POST)
// (POST)-[:TIENE]->(COMENTARIO)
// (USUARIO)-[:HACE]->(COMENTARIO)
// (USUARIO)-[:AUTORIZA]->(COMENTARIO)


// Creación de nodos vacíos para las etiquetas USUARIO, POST y COMENTARIO
// Se usan para inicializar la base de datos y evitar errores en consultas posteriores
CREATE (:USUARIO), (:POST), (:COMENTARIO);

// Crear índices adicionales para mejorar el rendimiento de las consultas
//equivalentes a PK
CREATE INDEX FOR (u:USUARIO) ON (u.idu);
CREATE INDEX FOR (p:POST) ON (p.idp);
CREATE INDEX FOR (c:COMENTARIO) ON (c.idc); //idc = p.idp + "-" + c.consec

// Crear restricciones para asegurar la unicidad de los identificadores
CREATE CONSTRAINT FOR (u:USUARIO) REQUIRE u.idu IS UNIQUE;
CREATE CONSTRAINT FOR (p:POST) REQUIRE p.idp IS UNIQUE;
CREATE CONSTRAINT FOR (c:COMENTARIO) REQUIRE c.consec IS UNIQUE;

//Nodo de ejemplo
CREATE (u0:USUARIO {idu: '0', nombre: 'John Doe'})