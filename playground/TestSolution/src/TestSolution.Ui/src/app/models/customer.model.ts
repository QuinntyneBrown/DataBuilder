/**
 * Represents a Customer model.
 */
export interface Customer {
  /** The unique identifier. */
  customerId: string;
  /** The name. */
  name: string;
  /** The email. */
  email: string;
  /** The age. */
  age: number;
  /** The isActive. */
  isActive: boolean;
  /** The createdDate. */
  createdDate: Date;
}

/**
 * Request model for creating a Customer.
 */
export interface CreateCustomerRequest {
  name: string;
  email: string;
  age: number;
  isActive: boolean;
  createdDate: Date;
}

/**
 * Request model for updating a Customer.
 */
export interface UpdateCustomerRequest {
  name: string;
  email: string;
  age: number;
  isActive: boolean;
  createdDate: Date;
}
