// Crear restricciones para asegurar la unicidad de los identificadores
CREATE CONSTRAINT IF NOT EXISTS FOR (u:USUARIO) REQUIRE u.idu IS UNIQUE;
CREATE CONSTRAINT IF NOT EXISTS FOR (p:POST) REQUIRE p.idp IS UNIQUE;
CREATE CONSTRAINT IF NOT EXISTS FOR (c:COMENTARIO) REQUIRE c.consec IS UNIQUE;

// Crear índices adicionales para mejorar el rendimiento de las consultas
//equivalentes a PK
//CREATE INDEX IF NOT EXISTS FOR (u:USUARIO) ON (u.idu);
//CREATE INDEX IF NOT EXISTS FOR (p:POST) ON (p.idp);
//CREATE INDEX IF NOT EXISTS FOR (c:COMENTARIO) ON (c.consec); 

//Inicialiar nodos de prueba
CREATE (u:USUARIO {idu: 'user1', nombre: 'Alice'});
CREATE (u:USUARIO {idu: 'user2', nombre: 'Bob'});
CREATE (u:USUARIO {idu: 'user3', nombre: 'Carol'});
CREATE (u:USUARIO {idu: 'user4', nombre: 'Dave'});
CREATE (u:USUARIO {idu: 'user5', nombre: 'Eve'});

CREATE (p:POST {idp: 'post1', titulo: 'First Post', contenido: 'This is the content of the first post.'});
CREATE (p:POST {idp: 'post2', titulo: 'Second Post', contenido: 'This is the content of the second post.'});
CREATE (p:POST {idp: 'post3', titulo: 'Third Post', contenido: 'This is the content of the third post.'});
CREATE (p:POST {idp: 'post4', titulo: 'Fourth Post', contenido: 'This is the content of the fourth post.'});
CREATE (p:POST {idp: 'post5', titulo: 'Fifth Post', contenido: 'This is the content of the fifth post.'});
CREATE (p:POST {idp: 'post6', titulo: 'Sixth Post', contenido: 'This is the content of the sixth post.'});
CREATE (p:POST {idp: 'post7', titulo: 'Seventh Post', contenido: 'This is the content of the seventh post.'});
CREATE (p:POST {idp: 'post8', titulo: 'Eighth Post', contenido: 'This is the content of the eighth post.'});
CREATE (p:POST {idp: 'post9', titulo: 'Ninth Post', contenido: 'This is the content of the ninth post.'});
CREATE (p:POST {idp: 'post10', titulo: 'Tenth Post', contenido: 'This is the content of the tenth post.'});

CREATE (c:COMENTARIO {consec:1, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Great post!'});
CREATE (c:COMENTARIO {consec:2, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Thanks for sharing.'});
CREATE (c:COMENTARIO {consec:3, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Interesting read.'});
CREATE (c:COMENTARIO {consec:4, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'I learned a lot.'});
CREATE (c:COMENTARIO {consec:5, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Well written!'});
CREATE (c:COMENTARIO {consec:6, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Looking forward to more posts like this.'});
CREATE (c:COMENTARIO {consec:7, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'This was very helpful.'});
CREATE (c:COMENTARIO {consec:8, fechorCom:datetime(), likeNotLike:FALSE, contenidoCom:'I disagree with some points.'});
CREATE (c:COMENTARIO {consec:9, fechorCom:datetime(), likeNotLike:FALSE, contenidoCom:'Can you provide more details?'});
CREATE (c:COMENTARIO {consec:10, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Excellent analysis.'});
CREATE (c:COMENTARIO {consec:11, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Very informative.'});
CREATE (c:COMENTARIO {consec:12, fechorCom:datetime(), likeNotLike:TRUE, contenidoCom:'Thanks for the insights.'});

// Crear relaciones entre nodos de prueba
// USUARIO -[PUBLICA]-> POST
UNWIND [
    {idu: 'user1', idp: 'post1'},
    {idu: 'user2', idp: 'post2'},
    {idu: 'user3', idp: 'post3'},
    {idu: 'user4', idp: 'post4'},
    {idu: 'user5', idp: 'post5'},
    {idu: 'user1', idp: 'post6'},
    {idu: 'user2', idp: 'post7'},
    {idu: 'user3', idp: 'post8'},
    {idu: 'user4', idp: 'post9'},
    {idu: 'user5', idp: 'post10'}
] AS rel_publica
MATCH (ux:USUARIO {idu: rel_publica.idu})
MATCH (px:POST {idp: rel_publica.idp})
CREATE (ux)-[:PUBLICA]->(px);

// USUARIO -[HACE]-> COMENTARIO
UNWIND [
    {idu: 'user2', consec: 1},
    {idu: 'user3', consec: 2},
    {idu: 'user4', consec: 3},
    {idu: 'user5', consec: 4},
    {idu: 'user1', consec: 5},
    {idu: 'user2', consec: 6},
    {idu: 'user3', consec: 7},
    {idu: 'user4', consec: 8},
    {idu: 'user5', consec: 9},
    {idu: 'user1', consec: 10},
    {idu: 'user2', consec: 11},
    {idu: 'user3', consec: 12}
] AS rel_hace
MATCH (ux:USUARIO {idu: rel_hace.idu})
MATCH (cx:COMENTARIO {consec: rel_hace.consec})
CREATE (ux)-[:HACE]->(cx);

// POST -[TIENE]-> COMENTARIO
UNWIND [
    {idp: 'post1', consec: 1},
    {idp: 'post1', consec: 2},
    {idp: 'post2', consec: 3},
    {idp: 'post3', consec: 4},
    {idp: 'post4', consec: 5},
    {idp: 'post5', consec: 6},
    {idp: 'post6', consec: 7},
    {idp: 'post7', consec: 8},
    {idp: 'post8', consec: 9},
    {idp: 'post9', consec: 10},
    {idp: 'post10', consec: 11},
    {idp: 'post10', consec: 12}
] AS rel_tiene
MATCH (px:POST {idp: rel_tiene.idp})
MATCH (cx:COMENTARIO {consec: rel_tiene.consec})
CREATE (px)-[:TIENE]->(cx);