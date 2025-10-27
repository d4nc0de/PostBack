// Crear restricciones para asegurar la unicidad de los identificadores
CREATE CONSTRAINT IF NOT EXISTS FOR (u:USUARIO) REQUIRE u.idu IS UNIQUE;
CREATE CONSTRAINT IF NOT EXISTS FOR (p:POST) REQUIRE p.idp IS UNIQUE;
CREATE CONSTRAINT IF NOT EXISTS FOR (c:COMENTARIO) REQUIRE c.consec IS UNIQUE;

// Crear índices adicionales para mejorar el rendimiento de las consultas
//equivalentes a PK
CREATE INDEX IF NOT EXISTS FOR (u:USUARIO) ON (u.idu);
CREATE INDEX IF NOT EXISTS FOR (p:POST) ON (p.idp);
//idc = p.idp + "-" + c.consec
CREATE INDEX IF NOT EXISTS FOR (c:COMENTARIO) ON (c.idc); 