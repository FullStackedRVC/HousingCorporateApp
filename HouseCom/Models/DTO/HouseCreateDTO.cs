﻿namespace HouseCom.Models.DTO
{
    public class HouseCreateDTO
    {
        
        
        public string Name { get; set; }
        public string Details { get; set; }
        public double Price { get; set; }
        public int Sqft { get; set; }
        public int Occupancy { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
