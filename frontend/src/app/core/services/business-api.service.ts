import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ActivityPoint,
  ApiEnvelope,
  Customer,
  CustomerOrder,
  DashboardStats,
  PageResult,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class BusinessApiService {
  private readonly baseUrl = environment.businessApiUrl;

  constructor(private readonly http: HttpClient) {}

  customers(): Observable<Customer[]> {
    return this.http
      .get<ApiEnvelope<Customer[]>>(`${this.baseUrl}/customers`)
      .pipe(map((response) => response.data));
  }

  createCustomer(value: Partial<Customer>): Observable<Customer> {
    return this.http
      .post<ApiEnvelope<Customer>>(`${this.baseUrl}/customers`, value)
      .pipe(map((response) => response.data));
  }

  updateCustomer(id: number, value: Partial<Customer>): Observable<Customer> {
    return this.http
      .put<ApiEnvelope<Customer>>(`${this.baseUrl}/customers/${id}`, value)
      .pipe(map((response) => response.data));
  }

  archiveCustomer(id: number): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/customers/${id}`);
  }

  orders(filters: Record<string, string | number | undefined> = {}): Observable<PageResult<CustomerOrder>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== '') {
        params = params.set(key, value);
      }
    }

    return this.http
      .get<ApiEnvelope<PageResult<CustomerOrder>>>(`${this.baseUrl}/orders`, { params })
      .pipe(map((response) => response.data));
  }

  createOrder(value: {
    customer_id: number;
    notes?: string;
    items: { description: string; quantity: number; unit_price: number }[];
  }): Observable<CustomerOrder> {
    return this.http
      .post<ApiEnvelope<CustomerOrder>>(`${this.baseUrl}/orders`, value)
      .pipe(map((response) => response.data));
  }

  updateOrder(id: number, value: {
    customer_id: number;
    notes?: string;
    items: { description: string; quantity: number; unit_price: number }[];
  }): Observable<CustomerOrder> {
    return this.http
      .put<ApiEnvelope<CustomerOrder>>(`${this.baseUrl}/orders/${id}`, value)
      .pipe(map((response) => response.data));
  }

  changeOrderStatus(id: number, action: 'complete' | 'cancel'): Observable<CustomerOrder> {
    return this.http
      .patch<ApiEnvelope<CustomerOrder>>(`${this.baseUrl}/orders/${id}/${action}`, {})
      .pipe(map((response) => response.data));
  }

  deleteOrder(id: number): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/orders/${id}`);
  }

  dashboardStats(): Observable<DashboardStats> {
    return this.http
      .get<ApiEnvelope<DashboardStats>>(`${this.baseUrl}/dashboard/stats`)
      .pipe(map((response) => response.data));
  }

  activityByMonth(): Observable<ActivityPoint[]> {
    return this.http
      .get<ApiEnvelope<ActivityPoint[]>>(`${this.baseUrl}/dashboard/orders-by-month`)
      .pipe(map((response) => response.data));
  }

  activityByDay(): Observable<ActivityPoint[]> {
    return this.http
      .get<ApiEnvelope<ActivityPoint[]>>(`${this.baseUrl}/dashboard/orders-by-day`)
      .pipe(map((response) => response.data));
  }
}