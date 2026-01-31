/**
 * Represents a Product model.
 */
export interface Product {
  /** The unique identifier. */
  productId: string;
  /** The name. */
  name: string;
  /** The description. */
  description: string;
  /** The price. */
  price: number;
  /** The inStock. */
  inStock: boolean;
  /** The releaseDate. */
  releaseDate: Date;
}

/**
 * Request model for creating a Product.
 */
export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  inStock: boolean;
  releaseDate: Date;
}

/**
 * Request model for updating a Product.
 */
export interface UpdateProductRequest {
  name: string;
  description: string;
  price: number;
  inStock: boolean;
  releaseDate: Date;
}
