export interface ApiEnvelope<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

export interface AuthSession {
  accountId: number;
  email: string;
  displayName: string;
  accessToken: string;
  accessExpiresAtUtc: string;
  refreshToken: string;
  refreshExpiresAtUtc: string;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  phone: string | null;
  address: string | null;
  is_active: boolean;
  created_at: string | null;
}

export type OrderStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled';

export interface OrderItem {
  id: number;
  description: string;
  quantity: number;
  unit_price: number;
}

export interface CustomerOrder {
  id: number;
  customer_id: number;
  customer_name?: string;
  status: OrderStatus;
  total: number;
  notes: string | null;
  created_at: string;
  updated_at: string;
  items: OrderItem[];
}

export interface PageResult<T> {
  items: T[];
  pagination: {
    total: number;
    per_page: number;
    current_page: number;
    last_page: number;
  };
}

export interface DashboardStats {
  total_orders: number;
  pending_orders: number;
  in_progress_orders: number;
  completed_orders: number;
  cancelled_orders: number;
  total_revenue: number;
  total_customers: number;
  active_customers: number;
}

export interface ActivityPoint {
  date: string;
  count: number;
  total: number;
}