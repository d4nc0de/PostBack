using System;

namespace HR.Models
{
    public class COMENTARIO
    {
        public int idp { get; set; }                
        public int consec { get; set; }            
        public int idu { get; set; }            
        public int? iduAutorizador { get; set; }
        public string contenidoCom { get; set; }  
        public DateTime fechorCom { get; set; }    
        public bool likeNotLike { get; set; }      
        public DateTime? fechorAut { get; set; }   
    }
}