// Crear restricciones para asegurar la unicidad de los identificadores
CREATE CONSTRAINT IF NOT EXISTS FOR (u:USUARIO) REQUIRE u.idu IS UNIQUE;
CREATE CONSTRAINT IF NOT EXISTS FOR (p:POST) REQUIRE p.idp IS UNIQUE;
CREATE CONSTRAINT IF NOT EXISTS FOR (c:COMENTARIO) REQUIRE c.consec IS UNIQUE;

// Crear índices adicionales para mejorar el rendimiento de las consultas
//equivalentes a PK
CREATE INDEX IF NOT EXISTS FOR (u:USUARIO) ON (u.idu);
CREATE INDEX IF NOT EXISTS FOR (p:POST) ON (p.idp);
CREATE INDEX IF NOT EXISTS FOR (c:COMENTARIO) ON (c.consec); 

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
MATCH (ux:USUARIO {idu:'user1'}), (px:POST {idp:'post1'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user2'}), (px:POST {idp:'post2'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user3'}), (px:POST {idp:'post3'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user4'}), (px:POST {idp:'post4'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user5'}), (px:POST {idp:'post5'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user1'}), (px:POST {idp:'post6'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user2'}), (px:POST {idp:'post7'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user3'}), (px:POST {idp:'post8'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user4'}), (px:POST {idp:'post9'}) CREATE (ux)-[:PUBLICA]->(px);
MATCH (ux:USUARIO {idu:'user5'}), (px:POST {idp:'post10'}) CREATE (ux)-[:PUBLICA]->(px);

MATCH (ux:USUARIO {idu:'user2'}), (cx:COMENTARIO {consec:1}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user3'}), (cx:COMENTARIO {consec:2}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user4'}), (cx:COMENTARIO {consec:3}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user5'}), (cx:COMENTARIO {consec:4}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user1'}), (cx:COMENTARIO {consec:5}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user2'}), (cx:COMENTARIO {consec:6}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user3'}), (cx:COMENTARIO {consec:7}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user4'}), (cx:COMENTARIO {consec:8}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user5'}), (cx:COMENTARIO {consec:9}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user1'}), (cx:COMENTARIO {consec:10}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user2'}), (cx:COMENTARIO {consec:11}) CREATE (ux)-[:HACE]->(cx);
MATCH (ux:USUARIO {idu:'user3'}), (cx:COMENTARIO {consec:12}) CREATE (ux)-[:HACE]->(cx);

MATCH (px:POST {idp:'post1'}), (cx:COMENTARIO {consec:1}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post1'}), (cx:COMENTARIO {consec:2}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post2'}), (cx:COMENTARIO {consec:3}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post3'}), (cx:COMENTARIO {consec:4}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post4'}), (cx:COMENTARIO {consec:5}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post5'}), (cx:COMENTARIO {consec:6}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post6'}), (cx:COMENTARIO {consec:7}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post7'}), (cx:COMENTARIO {consec:8}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post8'}), (cx:COMENTARIO {consec:9}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post9'}), (cx:COMENTARIO {consec:10}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post10'}), (cx:COMENTARIO {consec:11}) CREATE (px)-[:TIENE]->(cx);
MATCH (px:POST {idp:'post10'}), (cx:COMENTARIO {consec:12}) CREATE (px)-[:TIENE]->(cx);